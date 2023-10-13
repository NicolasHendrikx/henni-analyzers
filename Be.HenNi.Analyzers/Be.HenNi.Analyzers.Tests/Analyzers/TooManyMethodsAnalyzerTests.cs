using System.Threading.Tasks;
using Xunit;

using Verifier =
    Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        Be.HenNi.Analyzers.Analyzers.TooManyMethodsAnalyzer>;

namespace Be.HenNi.Analyzers.Tests.Analyzers;

public class TooManyMethodsAnalyzerTests
{
    [Fact]
    public async Task Should_Ignore_Interface()
    {
        const string text = @"
public interface IAdd 
{
  int Sum(int a, int b);
  int Minus(int a, int b);
  int Times(int a, int b);
}
";

        await Verifier.VerifyAnalyzerAsync(text).ConfigureAwait(false);
    }
    
    [Fact]
    public async Task Should_Report_Type_With_Too_Many_Methods()
    {
        const string text = @"
public class BadClass
{
    private static readonly int STRUCT_FIELD = 24;
    private System.DateTime _birthday = System.DateTime.Now;

    public int Age 
        => _birthday.Year;

    public string FirstName
        => ""Firstname"";

    public string LastName
        => ""Lastname""; 
}
";

        var expected = Verifier.Diagnostic()
            .WithLocation(2, 1)
            .WithArguments("BadClass", 2, 3);
        await Verifier.VerifyAnalyzerAsync(text, expected).ConfigureAwait(false);
    }

    [Fact]
    public async Task Should_Ignore_Type_With_Few_Methods()
    {
        const string text = @"
public class OkRecord
{
    public System.DateTime Birthday { get; set; }
    public int Age 
        => Birthday.Year;
}
";
        
        await Verifier.VerifyAnalyzerAsync(text).ConfigureAwait(false);        
    }
}