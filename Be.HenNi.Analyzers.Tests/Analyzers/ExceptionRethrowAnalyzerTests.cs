using System.Threading.Tasks;
using Xunit;

namespace Be.HenNi.Analyzers.Tests.Analyzers;

using Verifier =
    Be.HenNi.Analyzers.Tests.Verifiers.CSharpAnalyzerVerifier<
        Be.HenNi.Analyzers.Analyzers.ExceptionRethrowAnalyzer>;

public class ExceptionRethrowAnalyzerTests
{
    private const string Usings = @"
using System;
using System.IO;

public class ERA
{
    private void Work() { }
    private void Log(Exception e) { }
    private string Get() => null;
";

    private static string InMethod(string body)
        => $"{Usings}    public void Run()\n    {{\n{body}\n    }}\n}}\n";

    [Fact]
    public async Task Should_Alert_On_Catch_That_Only_Rethrows()
    {
        var source = InMethod(@"
        try { Work(); }
        {|HE0013:catch (IOException) { throw; }|}");
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Alert_On_Catch_That_Rethrows_The_Variable()
    {
        var source = InMethod(@"
        try { Work(); }
        {|HE0013:catch (Exception e) { throw e; }|}");
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Alert_On_General_Catch_That_Only_Rethrows()
    {
        var source = InMethod(@"
        try { Work(); }
        {|HE0013:catch { throw; }|}");
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Alert_When_The_Following_Catch_Is_Unrelated()
    {
        var source = InMethod(@"
        try { Work(); }
        {|HE0013:catch (IOException) { throw; }|}
        catch (ArgumentException e) { Log(e); }");
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Ignore_Rethrow_That_Shields_A_Following_Catch()
    {
        var source = InMethod(@"
        try { Work(); }
        catch (IOException) { throw; }
        catch (Exception e) { Log(e); }");
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Ignore_Rethrow_That_Shields_A_General_Catch()
    {
        var source = InMethod(@"
        try { Work(); }
        catch (IOException) { throw; }
        catch { }");
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Ignore_Catch_That_Does_More_Than_Rethrow()
    {
        var source = InMethod(@"
        try { Work(); }
        catch (Exception e) { Log(e); throw; }");
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Ignore_Conditional_Rethrow_And_Filters()
    {
        var source = InMethod(@"
        try { Work(); }
        catch (IOException e) when (e.Message != null) { throw; }
        catch (ArgumentException) { if (Get() == null) throw; }");
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Ignore_Wrapping_In_Another_Type()
    {
        var source = InMethod(@"
        try { Work(); }
        catch (IOException e) { throw new InvalidOperationException(""context"", e); }");
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Alert_On_New_Instance_Of_The_Caught_Type()
    {
        var source = InMethod(@"
        try { Work(); }
        catch (IOException e) { {|HE0014:throw new IOException(""context"", e);|} }");
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Alert_With_The_Caught_Type()
    {
        var source = InMethod(@"
        try { Work(); }
        catch (IOException e) { throw new IOException(""context"", e); }");
        var expected = Verifier.Diagnostic("HE0014")
            .WithSpan(14, 33, 14, 69)
            .WithArguments("System.IO.IOException");

        await Verifier.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task Should_Alert_On_New_Instance_In_A_Throw_Expression()
    {
        var source = InMethod(@"
        try { Work(); }
        catch (ArgumentException e) { var s = Get() ?? {|HE0014:throw new ArgumentException(""context"", e)|}; }");
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Alert_When_The_Throw_Is_Nested_In_The_Catch()
    {
        var source = InMethod(@"
        try { Work(); }
        catch (IOException e)
        {
            Log(e);
            if (Get() == null)
            {
                {|HE0014:throw new IOException(""context"");|}
            }
        }");
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Ignore_Other_Types_Base_Types_And_Derived_Types()
    {
        var source = InMethod(@"
        try { Work(); }
        catch (IOException e) { throw new Exception(""base"", e); }
        catch (ArgumentException e) { throw new ArgumentNullException(""derived"", e); }
        catch (InvalidOperationException e) { throw new NotSupportedException(""other"", e); }");
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Ignore_General_Catch_And_Throw_Outside_Catch()
    {
        var source = InMethod(@"
        try { throw new IOException(); }
        catch { throw new IOException(); }
        finally { }");
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Ignore_Throw_In_A_Nested_Function_Or_Another_Catch()
    {
        var source = InMethod(@"
        try { Work(); }
        catch (IOException e)
        {
            Action later = () => throw new IOException();
            void Local() => throw new IOException();
            try { Work(); }
            catch (ArgumentException) { throw new InvalidOperationException(); }
        }");
        await Verifier.VerifyAnalyzerAsync(source);
    }
}
