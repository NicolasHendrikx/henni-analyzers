using Be.HenNi.Analysis.Core.Metrics.Methods;
using Xunit;

namespace Be.HenNi.Analysis.Core.Tests.Metrics;

public class MethodMetricsTests
{
    [Fact]
    public void CYCLO_Reads_The_Method_Complexity()
        => Assert.Equal(13, new CyclomaticComplexity().Compute(Models.Of(Snippets.Flow, "Flow").Method("Compute")));

    [Fact]
    public void NCSS_Reads_The_Method_Statement_Count()
        => Assert.Equal(16, new NonCommentingSourceStatements().Compute(Models.Of(Snippets.Flow, "Flow").Method("Compute")));

    [Theory]
    [InlineData("Describe", 3)]
    [InlineData("Score", 1)]
    public void ATFD_Counts_Foreign_Attributes_Of_The_Method(string method, double expected)
        => Assert.Equal(expected, new AccessToForeignData().Compute(Models.Of(Snippets.Order, "Shop.Order").Method(method)));
}
