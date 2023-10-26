using System;
using System.Threading.Tasks;

using Xunit;
using Verifier =
    Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        Be.HenNi.Analyzers.Analyzers.LackOfCohesionOfMethodAnalyzer>;

namespace Be.HenNi.Analyzers.Tests.Analyzers;

public class LackOfCohesionOfMethodsAnalyzerTests
{
    [Fact]
    public async Task Should_Lack_Of_Cohesion_On_Method_Accessing_No_Fields()
    {
        const string text = @"
public class BadClass
{
    private static readonly int STRUCT_FIELD = 24;
    private int _field = 42;

    public int Age { get; set; } = 0;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;     
}
";

        var expected = Verifier.Diagnostic()
            .WithLocation(2, 1)
            .WithArguments("BadClass", 1.1, Double.PositiveInfinity);
        await Verifier.VerifyAnalyzerAsync(text, expected).ConfigureAwait(false);
    }

    [Fact]
    public async Task Should_Ignore_Type_With_Few_Fields()
    {
        const string text = @"
public struct Fraction
{
    public const string Message=""Hello World"";
        
    public int IntPart { get; set; }
    public short DecimalPart { get; set; }

    public double ToDouble
        => (double)IntPart/DecimalPart; 
}
";
        
        await Verifier.VerifyAnalyzerAsync(text).ConfigureAwait(false);        
    }
    
    [Fact(Skip = "Verifier needs an reference to dotnet5.0+")]
    public async Task Should_Ignore_Primary_Record()
    {
        const string text = @"
public record Fraction(int IntPart, int DecimalPart);
";

        await Verifier.VerifyAnalyzerAsync(text).ConfigureAwait(false);     
    }
    
}