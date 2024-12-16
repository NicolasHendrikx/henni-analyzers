using System.Threading.Tasks;
using Xunit;

namespace Be.HenNi.Analyzers.Tests.Analyzers;

using Verifier =
    Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        Be.HenNi.Analyzers.Analyzers.AvoidTightCouplingAnalyzer>;

public class AvoidTightCouplingTests
{
    private const string TightCoupledFieldDeclaration = @"
public class TCC 
{
    private System.Collections.Generic.List<string> _names = new();
}
";
    
    private const string TightCoupledPropertyDeclaration = @"
public class TCC 
{
    private System.Collections.Generic.HashSet<string> _names { get; set; }
}
";
    
    private const string TightCoupledIndexerDeclaration = @"
using System.Collections.Generic;

public class TCC 
{
    public Queue<string> this[string pos]
        => new Queue<string>();
}
";
    
    private const string LooselyCoupledFieldDeclaration = @"
using System.Collections.Generic;

public class LCC 
{
    private IList<string> _names = new List<string>();
}
";
    
    private const string IgnoreValueType = @"
using System.Collections.Generic;

public class LCC 
{
    private int _universal = 42;
    private string _name = ""Nicolas"";
    private IList<string> _names = new List<string>();
}
";
    
    private const string LooselyTypedIndexer = @"
using System.Collections.Generic;

public class LCC 
{
    public IEnumerable<int> this[string key]
        => new int[] { 1, 2, 3, 4 };
}
";
    
    private const string LooselyTypedDelegate = @"
using System;

public class LCC 
{
    private Action<int> _action = (i) 
        => Console.WriteLine(i);
}
";
    
   
    [Fact]
    public async Task Should_Ignore_On_Loosely_Coupled_Field()
    {
        await Verifier.VerifyAnalyzerAsync(LooselyCoupledFieldDeclaration).ConfigureAwait(false);
    }
    
    [Fact]
    public async Task Should_Ignore_Value_Type_Fields()
    {
        await Verifier.VerifyAnalyzerAsync(IgnoreValueType).ConfigureAwait(false);
    }
    
   
    [Fact]
    public async Task Should_Alert_On_Tight_Coupled_Indexer()
    {
        var expected = Verifier.Diagnostic()
            .WithSpan(6, 5, 7, 32)
            .WithArguments("System.Collections.Generic.Queue<string>");
        
        await Verifier.VerifyAnalyzerAsync(TightCoupledIndexerDeclaration, expected).ConfigureAwait(false);
    }
    
    [Fact]
    public async Task Should_Ignore_Loosely_Coupled_Indexer()
    {
        await Verifier.VerifyAnalyzerAsync(LooselyTypedIndexer).ConfigureAwait(false);
    }
    
    [Fact]
    public async Task Should_Ignore_Loosely_Coupled_Delegate()
    {
        await Verifier.VerifyAnalyzerAsync(LooselyTypedDelegate).ConfigureAwait(false);
    }
}