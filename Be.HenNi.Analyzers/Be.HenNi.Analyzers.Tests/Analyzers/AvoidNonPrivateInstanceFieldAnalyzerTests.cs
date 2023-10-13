using System.Threading.Tasks;
using Xunit;

namespace Be.HenNi.Analyzers.Tests.Analyzers;

using Verifier =
    Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        Be.HenNi.Analyzers.Analyzers.AvoidNonPrivateInstanceFieldAnalyzer>;

public class AvoidNonPrivateInstanceFieldAnalyzerTests
{
    [Fact]
    public async Task Should_Warn_When_Type_Has_Non_Private_Instance_Fields()
    {
        const string ThreeNonPrivateFields = @"
public class Clazz
{
    private readonly string _instanceField = string.Empty;
    protected readonly int wrongField = 1;
    public int anotherOne = 2;
    internal double yetAnother = 4.2;
}
";
        var expected = Verifier.Diagnostic()
            .WithLocation(2, 1)
            .WithArguments("Clazz", 3);
        await Verifier.VerifyAnalyzerAsync(ThreeNonPrivateFields, expected).ConfigureAwait(false);
    }

    [Fact]
    public async Task Should_Stay_Silent_When_Type_Has_All_Private_Instance_Fields()
    { 
        const string AllPrivateFields = @"
public record Records 
{
    private readonly string _instanceField = string.Empty;
    private readonly int _anotherOne = 0;
}
";

        await Verifier.VerifyAnalyzerAsync(AllPrivateFields).ConfigureAwait(false);
    }
}