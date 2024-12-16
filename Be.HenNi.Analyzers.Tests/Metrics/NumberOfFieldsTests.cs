using System.Linq;
using Be.HenNi.Analyzers.Constructions;
using Be.HenNi.Analyzers.Metrics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Be.HenNi.Analyzers.Tests.Metrics;

public class NumberOfFieldsTests
{
    const string EmptyClass = @"
public class Clazz
{
}
";

    private const string OneFieldClass = @"
public class Clazz
{
    private readonly string _instanceField = string.Empty;
}
";
    
    private const string OnePropertyRecord = @"
public record Records 
{
  public int Property { get;} = 0; 
}
";  
    private const string TwoPropertiesRecord = @"
public record Records(string FirstName, string LastName);
";

    private const string ManyFieldsStruct = @"
public struct MyStruct 
{
    private static readonly int STRUCT_FIELD = 24;
    private string _field = 42;

    public int Age { get; set } = 0;
    public string FirstName { get; set } = 0;
    public string LastName { get; set } = 0; 
}";

    [Theory]
    [InlineData(EmptyClass, 0.0)]
    [InlineData(OneFieldClass, 1.0)]
    [InlineData(OnePropertyRecord, 1.0)]
    [InlineData(TwoPropertiesRecord, 2.0)]
    [InlineData(ManyFieldsStruct, 4.0)]
    public void Computes_Number_Of_Instance_Fields(string typeDefinition, double expectedValue)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(typeDefinition);
        TypeDeclarationSyntax classRoot =
            tree.GetCompilationUnitRoot().Members.OfType<TypeDeclarationSyntax>().First();

        NumberOfFields computed = new NumberOfFields(new TypeDeclaration(classRoot));
        
        Assert.Equal(expectedValue, computed.Value);
    } 
}