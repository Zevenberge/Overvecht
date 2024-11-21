using FluentAssertions;
using Overvecht.CompilerTests.Models;

namespace Overvecht.CompilerTests;

public class AllArgsConstructorTests 
{
    [Fact]
    public void AllArgsConstructorAssignsPropertiesCorrectly()
    {
        var @object = new AllArgsCtor("Foo", 42);
        @object.Foo.Should().Be("Foo");
        @object.Bar.Should().Be(42);
    }
}
