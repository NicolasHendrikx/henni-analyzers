using Be.HenNi.Analysis.Core.Metrics;
using Be.HenNi.Analysis.Core.Metrics.Types;
using Be.HenNi.Analysis.Core.Model;
using Xunit;

namespace Be.HenNi.Analysis.Core.Tests.Metrics;

public class TypeMetricsTests
{
    private static double? Measure(IMetric<TypeModel> metric, string source, string type)
        => metric.Compute(Models.Of(source, type));

    [Theory]
    [InlineData(Snippets.Person, "Person", 3)]
    [InlineData(Snippets.Point, "Point", 2)]
    [InlineData(Snippets.Greeter, "Greeter", 2)]
    [InlineData(Snippets.Account, "Account", 3)]
    public void NOF_Counts_Instance_Attributes(string source, string type, double expected)
        => Assert.Equal(expected, Measure(new NumberOfFields(), source, type));

    [Theory]
    [InlineData(Snippets.Account, "Account", 6)]
    [InlineData(Snippets.Point, "Point", 0)]
    public void NOM_Excludes_Constructors_And_Auto_Accessors(string source, string type, double expected)
        => Assert.Equal(expected, Measure(new NumberOfMethods(), source, type));

    [Theory]
    [InlineData(Snippets.Person, "Person", 1)]
    [InlineData(Snippets.Account, "Account", 0)]
    public void NOPA_Counts_Non_Private_Fields(string source, string type, double expected)
        => Assert.Equal(expected, Measure(new NumberOfPublicAttributes(), source, type));

    [Theory]
    [InlineData(Snippets.Account, "Account", 5)]
    [InlineData(Snippets.Point, "Point", 4)]
    [InlineData(Snippets.Person, "Person", 2)]
    public void NOAM_Counts_Non_Private_Accessors(string source, string type, double expected)
        => Assert.Equal(expected, Measure(new NumberOfAccessorMethods(), source, type));

    [Theory]
    [InlineData(Snippets.Flow, "Flow", 16)]
    [InlineData(Snippets.Account, "Account", 7)]
    [InlineData(Snippets.Point, "Point", 0)]
    public void WMC_Sums_Complexity_Of_Written_Methods(string source, string type, double expected)
        => Assert.Equal(expected, Measure(new WeightedMethodCount(), source, type));

    [Theory]
    [InlineData(Snippets.Order, "Shop.Order", 3)]
    [InlineData(Snippets.Account, "Account", 0)]
    public void ATFD_Counts_Distinct_Foreign_Attributes(string source, string type, double expected)
        => Assert.Equal(expected, Measure(new AccessToForeignData(), source, type));

    [Fact]
    public void TCC_Counts_Pairs_Sharing_Attributes_Through_Calls()
        => Assert.Equal(4.0 / 6, Measure(new TightClassCohesion(), Snippets.Rectangle, "Rectangle")!.Value, 5);

    [Fact]
    public void TCC_Is_Undefined_With_Less_Than_Two_Methods()
        => Assert.Null(Measure(new TightClassCohesion(), Snippets.Greeter, "Greeter"));

    [Theory]
    [InlineData(Snippets.Account, "Account", 2.0 / 7)]
    [InlineData(Snippets.Point, "Point", 0)]
    [InlineData(Snippets.Person, "Person", 1.0 / 4)]
    public void WOC_Is_The_Share_Of_Functional_Public_Members(string source, string type, double expected)
        => Assert.Equal(expected, Measure(new WeightOfClass(), source, type)!.Value, 5);

    [Fact]
    public void LCOM_HS_Uses_Direct_Accesses()
        => Assert.Equal(0.75, Measure(new LackOfCohesionHendersonSellers(), Snippets.Rectangle, "Rectangle")!.Value, 5);

    [Fact]
    public void LCOM_HS_Is_Undefined_Without_Attributes()
        => Assert.Null(Measure(new LackOfCohesionHendersonSellers(), Snippets.Flow, "Flow"));

    [Fact]
    public void NCSS_Sums_Statements_Of_Methods()
        => Assert.Equal(17, Measure(new NonCommentingSourceStatements(), Snippets.Flow, "Flow"));
}
