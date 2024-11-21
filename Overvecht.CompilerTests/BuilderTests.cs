using FluentAssertions;
using Overvecht.CompilerTests.Models;

namespace Overvecht.CompilerTests;

public class BuilderTests
{
    [Fact]
    public void BuilderAssignsPropertiesCorrectly()
    {
        var @object = new Build.Builder()
            .WithFoo("Foo")
            .WithBar(42)
            .Build();
        @object.Foo.Should().Be("Foo");
        @object.Bar.Should().Be(42);
        
    }
}