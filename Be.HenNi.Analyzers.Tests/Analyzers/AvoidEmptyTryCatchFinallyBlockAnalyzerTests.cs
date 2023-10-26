namespace Be.HenNi.Analyzers.Tests.Analyzers;

using System.Threading.Tasks;

using Xunit;
using Verifier =
    Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        Be.HenNi.Analyzers.Analyzers.AvoidEmptyTryCatchFinallyBlockAnalyzer>;

public class AvoidEmptyTryCatchFinallyBlockAnalyzerTests
{
    private const string EmptyTry = @"
public class WrongClass 
{
    public static void Read() 
    {
        try
        {
        }
        catch(System.Exception ex)
        {
            System.Console.WriteLine(""HELLO"");
        }
    }
}
";

    private const string NonEmptyTryCatch = @"
public class WrongClass 
{
    public static void Read() 
    {
        try
        {
            System.Console.WriteLine(""HELLO"");
        }
        catch(System.Exception ex)
        {
            System.Console.WriteLine(""HELLO"");
        }
    }
}
";

    private const string EmptyCatch = @"
public class WrongClass 
{
    public static void Read() 
    {
        try
        {
            System.Console.WriteLine(""HELLO"");
        }
        catch(System.Exception ex)
        {
        }
    }
}
";
    
    private const string EmptyFinally = @"
public class WrongClass 
{
    public static void Read() 
    {
        try
        {
            System.Console.WriteLine(""HELLO"");
        }
        catch(System.Exception ex)
        {
            System.Console.WriteLine(""HELLO"");
        }
        finally 
        {
        }
    }
}
";

    [Fact]
    public async Task Should_Ignore_Non_Empty_Try_Catch_Statement()
    {
        await Verifier.VerifyAnalyzerAsync(NonEmptyTryCatch).ConfigureAwait(false);
    }
    
    [Fact]
    public async Task Should_Detect_Empty_Try_Statement()
    {
        var expected = Verifier
            .Diagnostic("HE0006")
            .WithSpan(6,9,12, 10);

        await Verifier.VerifyAnalyzerAsync(EmptyTry, expected).ConfigureAwait(false);
    }
        
    [Fact]
    public async Task Should_Detect_Empty_Catch_Statement()
    {
        var expected = Verifier
            .Diagnostic("HE0007")
            .WithSpan(10, 9, 12, 10);
        
        await Verifier.VerifyAnalyzerAsync(EmptyCatch, expected).ConfigureAwait(false);
    }
    
    [Fact]
    public async Task Should_Detect_Empty_Finally_Statement()
    {
        var expected = Verifier
            .Diagnostic("HE0008")
            .WithSpan(14, 9, 16, 10);
        
        await Verifier.VerifyAnalyzerAsync(EmptyFinally, expected).ConfigureAwait(false);
    }
}