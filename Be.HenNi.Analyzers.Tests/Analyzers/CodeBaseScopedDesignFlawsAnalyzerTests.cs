using System.Threading.Tasks;
using Xunit;

using Verifier =
    Be.HenNi.Analyzers.Tests.Verifiers.CSharpAnalyzerVerifier<
        Be.HenNi.Analyzers.Analyzers.CodeBaseScopedDesignFlawsAnalyzer>;

namespace Be.HenNi.Analyzers.Tests.Analyzers;

public class CodeBaseScopedDesignFlawsAnalyzerTests
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

    private static readonly Microsoft.CodeAnalysis.Testing.DiagnosticResult CustomerIsDataClass =
        Verifier.Diagnostic("HE0011")
            .WithLocation(1, 14)
            .WithArguments("Customer", "Manager.Report", "WOC=0 (< 0.33), NOPA+NOAM=6 (> 3), WMC=0 (< 31), NDCM=1 (≥ 1)");

    [Fact]
    public async Task Should_Report_Data_Class_With_Default_Thresholds()
        => await Verifier.VerifyAnalyzerAsync(Manager, CustomerIsDataClass);

    [Fact]
    public async Task Should_Ignore_Dto_That_Is_Only_Mapped()
    {
        const string text = """
            public record OrderLine(string Product, int Quantity);
            public record Order(string Product, decimal Total, int Lines);

            public class Mapper
            {
                public Order ToOrder(OrderLine line) => new Order(line.Product, 0, line.Quantity);
            }
            """;

        await Verifier.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task Should_Not_Report_Data_Class_When_Disabled_By_Threshold()
        => await Verifier.VerifyAnalyzerAsync(Manager, "dotnet_diagnostic.HE0011.min_complexity_of_client = 4");
}
