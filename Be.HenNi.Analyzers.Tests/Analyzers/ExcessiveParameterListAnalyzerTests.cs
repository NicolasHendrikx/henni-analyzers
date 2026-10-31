using System.Threading.Tasks;
using Xunit;

namespace Be.HenNi.Analyzers.Tests.Analyzers;

using Verifier =
    Be.HenNi.Analyzers.Tests.Verifiers.CSharpAnalyzerVerifier<
        Be.HenNi.Analyzers.Analyzers.ExcessiveParameterListAnalyzer>;

public class ExcessiveParameterListAnalyzerTests
{
    [Fact]
    public async Task Should_Alert_On_Method_With_Six_Parameters()
    {
        const string source = @"
public class EPL
{
    public void Move(int a, int b, int c, int d, int e, int f) { }
}
";
        var expected = Verifier.Diagnostic()
            .WithSpan(4, 21, 4, 63)
            .WithArguments("EPL.Move", 5, 6);

        await Verifier.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task Should_Ignore_Method_With_Five_Parameters()
    {
        const string source = @"
public class EPL
{
    public void Move(int a, int b, int c, int d, int e) { }
}
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Alert_Whatever_The_Visibility()
    {
        const string source = @"
public class EPL
{
    private static void Move{|HE0012:(int a, int b, int c, int d, int e, int f)|} { }
}
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Alert_On_Constructor_Operator_Conversion_And_Indexer()
    {
        const string source = @"
public class EPL
{
    public EPL{|HE0012:(int a, int b, int c, int d, int e, int f)|} { }
    public static EPL operator +{|HE0012:(EPL a, EPL b)|} => a;
    public static explicit operator EPL{|HE0012:(int a)|} => null;
    public int this{|HE0012:[int a, int b, int c, int d, int e, int f]|} => 0;
}
";
        await Verifier.VerifyAnalyzerAsync(source, "dotnet_diagnostic.HE0012.threshold = 0");
    }

    [Fact]
    public async Task Should_Name_The_Constructor_After_Its_Type()
    {
        const string source = @"
public class EPL
{
    public EPL(int a, int b, int c, int d, int e, int f) { }
}
";
        var expected = Verifier.Diagnostic()
            .WithSpan(4, 15, 4, 57)
            .WithArguments("EPL", 5, 6);

        await Verifier.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task Should_Alert_On_Delegate()
    {
        const string source = @"
public delegate void Handler{|HE0012:(int a, int b, int c, int d, int e, int f)|};
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Alert_On_Record_And_Primary_Constructor()
    {
        const string source = @"
public record Position{|HE0012:(int A, int B, int C, int D, int E, int F)|};
public record struct Point{|HE0012:(int A, int B, int C, int D, int E, int F)|};
public class Holder{|HE0012:(int a, int b, int c, int d, int e, int f)|};
public record Small(int A, int B);
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Count_Optional_And_Params_Parameters()
    {
        const string source = @"
public class EPL
{
    public void Move{|HE0012:(int a, int b, int c, int d = 1, int e = 2, params int[] f)|} { }
}
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Ignore_Overrides_And_Interface_Implementations()
    {
        const string source = @"
public interface IMover
{
    void Move{|HE0012:(int a, int b, int c, int d, int e, int f)|};
    void Slide{|HE0012:(int a, int b, int c, int d, int e, int f)|};
}

public abstract class Base
{
    public abstract void Fill{|HE0012:(int a, int b, int c, int d, int e, int f)|};
}

public class Mover : Base, IMover
{
    public void Move(int a, int b, int c, int d, int e, int f) { }
    void IMover.Slide(int a, int b, int c, int d, int e, int f) { }
    public override void Fill(int a, int b, int c, int d, int e, int f) { }
}
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Use_Configured_Threshold()
    {
        const string source = @"
public class EPL
{
    public void Few{|HE0012:(int a, int b, int c)|} { }
    public void Fewer(int a, int b) { }
}
";
        await Verifier.VerifyAnalyzerAsync(source, "dotnet_diagnostic.HE0012.threshold = 2");
    }

    [Fact]
    public async Task Should_Ignore_Lambdas_And_Local_Functions()
    {
        const string source = @"
public class EPL
{
    public void Run()
    {
        System.Func<int, int, int, int, int, int, int> sum = (a, b, c, d, e, f) => a + b + c + d + e + f;
        int Local(int a, int b, int c, int d, int e, int f) => a;
    }
}
";
        await Verifier.VerifyAnalyzerAsync(source);
    }
}
