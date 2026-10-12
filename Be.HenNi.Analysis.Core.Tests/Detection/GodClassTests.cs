using System.Linq;
using Be.HenNi.Analysis.Core.Detection.Smells;
using Xunit;

namespace Be.HenNi.Analysis.Core.Tests.Detection;

/// <summary>
/// Thresholds are lowered so that the snippets stay readable.
/// </summary>
public class GodClassTests
{
    private const string Manager = """
        public class Customer
        {
            public string Name { get; set; } = "";
            public int Points { get; set; }
            public string Email { get; set; } = "";
        }

        public class Manager
        {
            private int _a;
            private int _b;
            private string _log = "";

            public string Report(Customer c)
            {
                if (c.Points > 10 && c.Name != "") return c.Email;
                return c.Name;
            }

            public int Sum() { return _a + _b; }

            public void Log(string message) { if (message.Length > 0) _log += message; }
        }
        """;

    private static GodClass LoweredGodClass()
        => new() { FewForeignData = 2, VeryHighComplexity = 5 };

    [Fact]
    public void Default_Thresholds_Come_From_The_Literature()
    {
        var strategy = new GodClass();

        Assert.Equal((5, 47, 1.0 / 3), (strategy.FewForeignData, strategy.VeryHighComplexity, strategy.LowCohesion));
    }

    [Fact]
    public void Detects_Complex_Uncohesive_Type_Using_Foreign_Data()
    {
        var verdict = LoweredGodClass().Evaluate(Models.Of(Manager, "Manager"));

        Assert.True(verdict.IsSatisfied);
        Assert.Equal("ATFD=3 (> 2), WMC=6 (≥ 5), TCC=0 (< 0.33)", verdict.ToString());
    }

    [Fact]
    public void Ignores_Cohesive_Type()
    {
        var verdict = LoweredGodClass().Evaluate(Models.Of(Snippets.Rectangle, "Rectangle"));

        Assert.False(verdict.IsSatisfied);
        Assert.Equal("ATFD", verdict.Evidence.Single().Acronym);
    }

    [Fact]
    public void Ignores_Type_Below_Default_Thresholds()
        => Assert.False(new GodClass().Evaluate(Models.Of(Manager, "Manager")).IsSatisfied);
}
