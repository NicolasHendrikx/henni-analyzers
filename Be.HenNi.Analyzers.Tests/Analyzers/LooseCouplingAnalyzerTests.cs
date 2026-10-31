using System.Threading.Tasks;
using Xunit;

namespace Be.HenNi.Analyzers.Tests.Analyzers;

using Verifier =
    Be.HenNi.Analyzers.Tests.Verifiers.CSharpAnalyzerVerifier<
        Be.HenNi.Analyzers.Analyzers.LooseCouplingAnalyzer>;

public class LooseCouplingAnalyzerTests
{
    [Fact]
    public async Task Should_Alert_On_Public_Field()
    {
        const string source = @"
using System.Collections.Generic;

public class TCC
{
    public {|HE0009:List<string>|} Names = new();
}
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Alert_With_The_Offending_Type()
    {
        const string source = @"
using System.Collections.Generic;

public class TCC
{
    public Queue<string> Names = new();
}
";
        var expected = Verifier.Diagnostic()
            .WithSpan(6, 12, 6, 25)
            .WithArguments("System.Collections.Generic.Queue<string>");

        await Verifier.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task Should_Alert_On_Protected_Property()
    {
        const string source = @"
using System.Collections.Generic;

public class TCC
{
    protected {|HE0009:HashSet<string>|} Names { get; set; }
}
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Alert_On_Indexer_And_Its_Parameters()
    {
        const string source = @"
using System.Collections.Generic;

public class TCC
{
    public {|HE0009:Queue<string>|} this[{|HE0009:List<int>|} keys] => new();
}
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Alert_On_Method_Return_Type_And_Parameters()
    {
        const string source = @"
using System.Collections.Generic;

public class TCC
{
    public {|HE0009:Dictionary<string, int>|} Count({|HE0009:List<string>|} names, IEnumerable<string> others, ref {|HE0009:HashSet<int>|} set)
        => new();
}
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Alert_On_Constructor_And_Operator()
    {
        const string source = @"
using System.Collections.Generic;

public class TCC
{
    public TCC({|HE0009:List<string>|} names) { }
    public static TCC operator +(TCC left, {|HE0009:Stack<int>|} right) => left;
    public static implicit operator {|HE0009:List<int>|}(TCC value) => new();
}
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Alert_On_Interface_Member()
    {
        const string source = @"
using System.Collections.Generic;

public interface ITCC
{
    {|HE0009:List<string>|} Names { get; }
}
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Alert_Once_On_Nested_Collections()
    {
        const string source = @"
using System.Collections.Generic;

public class TCC
{
    public {|HE0009:List<List<int>>|} Matrix { get; }
}
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Alert_On_Collection_Nested_In_Type_Arguments()
    {
        const string source = @"
using System.Collections.Generic;
using System.Threading.Tasks;

public class TCC
{
    public Task<{|HE0009:List<int>|}> LoadAsync() => Task.FromResult(new List<int>());
    public IEnumerable<{|HE0009:HashSet<int>|}> Groups { get; }
    public {|HE0009:List<int>|}[] Lists { get; }
    public ({|HE0009:List<int>|} Names, int Count) Pair { get; }
}
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Alert_On_Positional_Record_Parameters()
    {
        const string source = @"
using System.Collections.Generic;

public record Inventory({|HE0009:List<string>|} Items, IReadOnlyList<string> Tags);
public record struct Pair({|HE0009:Dictionary<int, int>|} Values);
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Alert_On_Record_Members()
    {
        const string source = @"
using System.Collections.Generic;

public record Inventory
{
    public {|HE0009:List<string>|} Items { get; init; }
    public void Add({|HE0009:HashSet<string>|} items) { }
}
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Alert_On_Primary_Constructor_Parameters()
    {
        const string source = @"
using System.Collections.Generic;

public class TCC({|HE0009:List<string>|} names)
{
    private readonly List<string> _names = names;
}
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Alert_On_User_Defined_Concrete_Collection()
    {
        const string source = @"
using System.Collections;
using System.Collections.Generic;

public class Bag : IEnumerable<int>
{
    public IEnumerator<int> GetEnumerator() => throw new System.NotImplementedException();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class TCC
{
    public {|HE0009:Bag|} Bag { get; }
}
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Ignore_Interfaces_And_Observable_Collections()
    {
        const string source = @"
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class LCC
{
    public IList<string> Names { get; } = new List<string>();
    public IReadOnlyDictionary<string, int> Counts { get; }
    public ISet<int> Set { get; }
    public IEnumerable<string> Enumerate(IEnumerable<string> names) => names;
    public ObservableCollection<string> Observed { get; } = new();
    public ReadOnlyObservableCollection<string> ReadOnlyObserved { get; }
    public string[] Array { get; }
}
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Ignore_Types_Derived_From_Observable_Collection()
    {
        const string source = @"
using System.Collections.ObjectModel;

public class Playlists : ObservableCollection<string> { }

public class LCC
{
    public Playlists Playlists { get; }
}
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Ignore_Value_Types_Strings_Delegates_And_Abstract_Types()
    {
        const string source = @"
using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

public class LCC
{
    public int Universal = 42;
    public string Name { get; }
    public ImmutableArray<int> Immutable { get; }
    public Span<int> Span => default;
    public Action<int> Action { get; }
    public KeyedCollection<string, int> Keyed { get; }
}
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Ignore_Concrete_Types_That_Are_Not_Collections()
    {
        const string source = @"
using System.Text;

public class Person { }

public class LCC
{
    public Person Owner { get; }
    public StringBuilder Builder { get; }
}
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Ignore_Private_Members()
    {
        const string source = @"
using System.Collections.Generic;

public class LCC
{
    private List<string> _names = new();
    private List<string> Names { get; }
    private void Add(List<string> names) { }
    private class Nested
    {
        public List<string> Names { get; }
    }
}
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Ignore_Internal_Members_By_Default()
    {
        const string source = @"
using System.Collections.Generic;

internal class Hidden
{
    public List<string> Names { get; }
}

public class LCC
{
    internal List<string> Names { get; }
}
";
        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Should_Alert_On_Internal_Members_When_Configured()
    {
        const string source = @"
using System.Collections.Generic;

internal class Hidden
{
    public {|HE0009:List<string>|} Names { get; }
}

public class LCC
{
    internal {|HE0009:List<string>|} Names { get; }
    private List<string> _names;
}
";
        await Verifier.VerifyAnalyzerAsync(source, "dotnet_diagnostic.HE0009.include_internal = true");
    }

    [Fact]
    public async Task Should_Ignore_Overrides_And_Interface_Implementations()
    {
        const string source = @"
using System.Collections.Generic;

public interface IHolder
{
    List<string> Names { get; }
    void Add(List<string> names);
    List<int> Numbers();
}

public abstract class Base
{
    public abstract void Fill(List<string> names);
}

public class Holder : Base, IHolder
{
    public List<string> Names { get; }
    public void Add(List<string> names) { }
    List<int> IHolder.Numbers() => null;
    public override void Fill(List<string> names) { }
}
";
        // Only the declarations of the interface and of the abstract member are reported.
        await Verifier.VerifyAnalyzerAsync(source.Replace("    List<string> Names { get; }\n    void Add(List<string> names);\n    List<int> Numbers();",
                "    {|HE0009:List<string>|} Names { get; }\n    void Add({|HE0009:List<string>|} names);\n    {|HE0009:List<int>|} Numbers();")
            .Replace("public abstract void Fill(List<string> names);", "public abstract void Fill({|HE0009:List<string>|} names);"));
    }

    [Fact]
    public async Task Should_Ignore_Allowed_Types()
    {
        const string source = @"
using System.Collections.Generic;

public class LCC
{
    public List<string> Names { get; }
    public {|HE0009:HashSet<string>|} Set { get; }
}
";
        await Verifier.VerifyAnalyzerAsync(source, "dotnet_diagnostic.HE0009.allowed_types = System.Collections.Generic.List, Some.Other.Type");
    }
}
