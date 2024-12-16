using System.Linq;
using Be.HenNi.Analyzers.Constructions;
using Be.HenNi.Analyzers.Metrics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Be.HenNi.Analyzers.Tests.Metrics;

public class NumberOfMethodsTests
{
    private const string SimpleClass = @"
public class SimpleClass 
{
    public static int Method1()
       => 1;
    public static int Property1
       => 2;
}    
";
    
    private const string IndexersClass = @"
public class SimpleClass 
{
    public static int Method1()
       => 1;
    public static int Property1
       => 2;

    public string this[int pos] 
    {
       get => string.Empty;
       set 
       {
          var test = value;
          return;
       }
    }
}    
";
    
    private const string AutoEventClass = @"
public class SimpleClass 
{
    public static int Method1()
       => 1;
    public static int Property1
       => 2;

    public event EventHandler<int> ThisChanged
    {
        add => System.Console.WriteLine(""Handler added"");
    }

    public string this[int pos] 
         => string.Empty;       
}    
";
    
    [Theory]
    [InlineData(SimpleClass, 2)]
    [InlineData(IndexersClass, 4)]
    [InlineData(AutoEventClass, 4)]   
    public void Should_Computes_Number_Of_Methods(string typeDefinition, double expectedValue)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(typeDefinition);
        TypeDeclarationSyntax classRoot =
            tree.GetCompilationUnitRoot().Members.OfType<TypeDeclarationSyntax>().First();

        var computed = new NumberOfMethods(new TypeDeclaration(classRoot));
        
        Assert.Equal(expectedValue, computed.Value);
    } 
}