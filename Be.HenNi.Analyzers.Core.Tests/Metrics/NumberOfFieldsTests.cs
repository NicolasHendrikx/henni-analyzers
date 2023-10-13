using Be.HenNi.Analyzers.Core.Constructions;
using Be.HenNi.Analyzers.Core.Metrics;
using NSubstitute;
using Xunit;

namespace Be.HenNi.Analyzers.Core.Tests.Metrics;

public class NumberOfFieldsTests
{
    [Fact]
    void Should_Returns_The_Numbers_Of_Fields()
    {
        var someType = Substitute.For<ITypeDeclaration>();
        var fields = new[] { AFieldNamed("f1") };
        someType.InstanceFields.Returns(fields);

        var numberOfFields = new NumberOfFields(someType);
        
        Assert.Equal(1.0, numberOfFields.Value);
    }

    private static IFieldDeclaration AFieldNamed(string fieldName)
    {
        var field = Substitute.For<IFieldDeclaration>();
        
        field.Identifier.Returns(fieldName);
        
        return field;
    }
}