using System;
using System.Linq;
using Be.HenNi.Analyzers.Metrics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Be.HenNi.Analyzers.Tests.Metrics;

public class CohesionTests
{
    const string EmptyFieldClass = @"
public class EmptyFieldClass
{
}
";

    private const string EmptyMethodsClass = @"
public class EmptyMethodClass
{
    public string Name { get; set; }
    public string FirstName { get; set; }
}
";
    
    private const string VeryCohesiveRecord = @"
public record Records 
{
  public System.DateTime BirthDate { get; set; }
  public int Age
    => (System.DateTime.Now - BirthDate).Years;
}
";  
    private const string UncohesiveRecord = @"
public record Records(string FirstName, string LastName)
{ 
    public string SayHello()
      => ""Bonjour toi"";
}";

    private const string FairlyUncohesiveStruct = @"
public struct MyStruct 
{
    private static readonly int STRUCT_FIELD = 24;
    private string _field = 42;

    public int Age
        => _field * 10;

    public string FirstName { get; set } = 0;
    public string LastName { get; set } = 0; 
}";
   
    private const string FairlyCohesiveStruct = @"
public struct MyStruct 
{    
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;

    public string Fullname
       => Firstname + "" ""+Lastname;

    public string FistnameInitial
       => Firstname[0];
}";

    [Theory]
    [InlineData(EmptyFieldClass, Double.NegativeInfinity)]
    [InlineData(EmptyMethodsClass, Double.PositiveInfinity)]
    [InlineData(VeryCohesiveRecord, 1.0)]    
    [InlineData(UncohesiveRecord, 0.0)]
    [InlineData(FairlyUncohesiveStruct, 0.333333)]
    [InlineData(FairlyCohesiveStruct, 0.75)]
    public void Computes_The_Cohesion_Of_Fields_With_Method(string typeDefinition, double expectedValue)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(typeDefinition);
        TypeDeclarationSyntax classRoot =
            tree.GetCompilationUnitRoot().Members.OfType<TypeDeclarationSyntax>().First();

        CKCohesion computed = new CKCohesion(classRoot);
        
        Assert.Equal(expectedValue, computed.Value, 5);
    } 
}