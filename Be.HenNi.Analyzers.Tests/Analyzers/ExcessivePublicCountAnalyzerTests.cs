using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Be.HenNi.Analyzers.Tests.Analyzers;

using Verifier =
    Be.HenNi.Analyzers.Tests.Verifiers.CSharpAnalyzerVerifier<
        Be.HenNi.Analyzers.Analyzers.ExcessivePublicCountAnalyzer>;

public class ExcessivePublicCountAnalyzerTests
{
    private const string Threshold2 = "dotnet_diagnostic.HE0003.threshold = 2";

    [Fact]
    public async Task Should_Alert_With_Name_Threshold_And_Count()
    {
        const string source = @"
public class EPC
{
    public int A;
    public int B { get; set; }
    public void C() { }
}
";
        var expected = Verifier.Diagnostic()
            .WithSpan(2, 14, 2, 17)
            .WithArguments("EPC", 2, 3);

        await Verifier.VerifyAnalyzerAsync(source, Threshold2, expected);
    }

    [Fact]
    public async Task Should_Ignore_Type_At_Threshold()
    {
        const string source = @"
public class EPC
{
    public int A;
    public void C() { }
}
";
        await Verifier.VerifyAnalyzerAsync(source, Threshold2);
    }

    [Fact]
    public async Task Should_Use_Default_Threshold_Of_20()
    {
        var members = string.Join("\n", Enumerable.Range(0, 20).Select(i => $"    public void M{i}() {{ }}"));
        await Verifier.VerifyAnalyzerAsync($"public class EPC\n{{\n{members}\n}}\n");

        var tooMany = $"public class {{|HE0003:EPC|}}\n{{\n{members}\n    public void Last() {{ }}\n}}\n";
        await Verifier.VerifyAnalyzerAsync(tooMany);
    }

    [Fact]
    public async Task Should_Count_Each_Kind_Of_Member_Once()
    {
        const string source = @"
public class EPC
{
    public int Field1, Field2;
    public const int Constant = 1;
    public static int StaticField;
    public int Auto { get; set; }
    public int Written { get => 1; set { } }
    public int this[int index] => 0;
    public event System.Action Changed;
    public event System.Action Custom { add { } remove { } }
    public void Method() { }
    public static EPC operator +(EPC left, EPC right) => left;
    public static implicit operator int(EPC value) => 0;
}
";
        // 2 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 = 12
        var expected = Verifier.Diagnostic().WithLocation(2, 14).WithArguments("EPC", 11, 12);
        await Verifier.VerifyAnalyzerAsync(source, "dotnet_diagnostic.HE0003.threshold = 11", expected);
    }

    [Fact]
    public async Task Should_Ignore_Constructors_Finalizers_Nested_Types_And_Private_Members()
    {
        const string source = @"
public class EPC
{
    public EPC() { }
    public EPC(int a) { }
    static EPC() { }
    ~EPC() { }
    public class Nested { }
    private int _field;
    private int Property { get; set; }
    private void Method() { }
    public int A;
}
";
        await Verifier.VerifyAnalyzerAsync(source, "dotnet_diagnostic.HE0003.threshold = 1");
    }

    [Fact]
    public async Task Should_Count_Internal_Members_By_Default()
    {
        const string source = @"
public class {|HE0003:EPC|}
{
    public int A;
    internal int B;
    protected internal int C;
    protected int D;
    private protected int E;
}
";
        await Verifier.VerifyAnalyzerAsync(source, "dotnet_diagnostic.HE0003.threshold = 2");
    }

    [Fact]
    public async Task Should_Ignore_Internal_Members_When_Configured()
    {
        const string source = @"
public class EPC
{
    public int A;
    internal int B;
    internal protected int C;
    protected int D;
}
";
        await Verifier.VerifyAnalyzerAsync(source, "dotnet_diagnostic.HE0003.threshold = 1\ndotnet_diagnostic.HE0003.include_internal = false");
    }

    [Fact]
    public async Task Should_Count_Protected_Members_When_Configured()
    {
        const string source = @"
public class {|HE0003:EPC|}
{
    public int A;
    protected int B;
    private protected int C;
}
";
        await Verifier.VerifyAnalyzerAsync(source, "dotnet_diagnostic.HE0003.threshold = 2\ndotnet_diagnostic.HE0003.include_protected = true");
    }

    [Fact]
    public async Task Should_Count_Members_Of_Structs_Interfaces_And_Partial_Types()
    {
        const string source = @"
public struct {|HE0003:S|}
{
    public int A;
    public int B;
    public int C;
}

public interface {|HE0003:I|}
{
    void A();
    int B { get; }
    void C();
}

public partial class {|HE0003:P|}
{
    public int A;
}

public partial class P
{
    public int B;
    public int C;
}
";
        await Verifier.VerifyAnalyzerAsync(source, Threshold2);
    }

    [Fact]
    public async Task Should_Count_Positional_Record_Properties_But_Not_Synthesized_Members()
    {
        const string source = @"
public record {|HE0003:Position|}(int A, int B, int C);
public record Small(int A, int B);
";
        await Verifier.VerifyAnalyzerAsync(source, Threshold2);
    }

    [Fact]
    public async Task Should_Ignore_Explicit_Interface_Implementations()
    {
        const string source = @"
public interface I { void A(); void B(); void C(); }

public class EPC : I
{
    void I.A() { }
    void I.B() { }
    void I.C() { }
    public int D;
}
";
        // The interface counts 3 members, the class only the public one.
        await Verifier.VerifyAnalyzerAsync(source, "dotnet_diagnostic.HE0003.threshold = 3");
    }

    [Fact]
    public async Task Should_Ignore_Members_Declared_Only_In_Generated_Code()
    {
        var sources = new[]
        {
            ("Written.cs", @"
public partial class EPC
{
    public int A;
    public int B;
}
"),
            ("Toolkit.g.cs", @"
public partial class EPC
{
    public int C { get; set; }
    public int D { get; set; }
}
")
        };
        await Verifier.VerifyAnalyzerAsync(sources, Threshold2);
    }
}
