using System.Threading.Tasks;
using Xunit;

using Verifier =
    Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        Be.HenNi.Analyzers.Analyzers.TooManyFieldsAnalyzer>;

namespace Be.HenNi.Analyzers.Tests.Analyzers;

public class TooManyFieldsAnalyzerTests
{
    [Fact]
    public async Task Should_Report_Type_With_Too_Many_Fields()
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
            .WithArguments("BadClass", 2, 4);
        await Verifier.VerifyAnalyzerAsync(text, expected).ConfigureAwait(false);
    }

    [Fact]
    public async Task Should_Ignore_Type_With_Few_Fields()
    {
        const string text = @"
public struct OkStruct
{
    public const string Message=""Hello World"";
        
    public int IntPart { get; set; }
    public short DecimalPart { get; set; } 
}
";
        
        await Verifier.VerifyAnalyzerAsync(text).ConfigureAwait(false);        
    }
}