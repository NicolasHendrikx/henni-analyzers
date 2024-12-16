using System.Linq;
using Be.HenNi.Analyzers.Constructions;
using Be.HenNi.Analyzers.Metrics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Be.HenNi.Analyzers.Tests.Metrics;

public class NumberOfNonPrivateInstanceFieldsTests
{
    const string EmptyClass = @"
public class Clazz
{
}
";

    private const string ThreeNonPrivateFields = @"
public class Clazz
{
    private readonly string _instanceField = string.Empty;
    protected readonly int wrongField = 1;
    public int anotherOne = 2;
    internal double yetAnother = 4.2;
}
";
    
    private const string AllPrivateFields = @"
public record Records 
{
    private readonly string _instanceField = string.Empty;
    private readonly int _anotherOne = 0;
}
";  

    [Theory]
    [InlineData(EmptyClass, 0)]
    [InlineData(AllPrivateFields, 0)]    
    [InlineData(ThreeNonPrivateFields, 3)]    
    public void Computes_Number_Of_Instance_Fields(string typeDefinition, double expectedValue)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(typeDefinition);
        TypeDeclarationSyntax classRoot =
            tree.GetCompilationUnitRoot().Members.OfType<TypeDeclarationSyntax>().First();

        var computed = new NumberOfNonPrivateInstanceFields(new TypeDeclaration(classRoot));
        
        Assert.Equal(expectedValue, computed.Value);
    } 
}