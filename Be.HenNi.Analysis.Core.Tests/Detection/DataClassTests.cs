using System.Linq;
using Be.HenNi.Analysis.Core.Detection.Smells;
using Be.HenNi.Analysis.Core.Metrics.Types;
using Xunit;

namespace Be.HenNi.Analysis.Core.Tests.Detection;

public class DataClassTests
{
    private const string OrderService = """
        public class OrderService
        {
            public bool CanShip(Order order, decimal limit)
            {
                if (order.Status == "Paid" && order.Total <= limit) return true;
                return false;
            }
        }
        """;

    private const string Mapper = """
        public record OrderLine(string Product, int Quantity);

        public class Mapper
        {
            public Order ToOrder(OrderLine line) => new Order(line.Product, 0, line.Quantity);
        }
        """;

    private const string OrderRecord = "public record Order(string Status, decimal Total, int Lines);";

    [Theory]
    [InlineData(OrderRecord)]
    [InlineData("public record struct Order(string Status, decimal Total, int Lines);")]
    [InlineData("public class Order { public string Status { get; set; } = \"\"; public decimal Total { get; set; } public int Lines { get; set; } }")]
    [InlineData("public struct Order { public string Status { get; set; } public decimal Total { get; set; } public int Lines { get; set; } }")]
    public void Detects_Anemic_Type_Whose_Data_Are_Exploited_Elsewhere_Whatever_Its_Kind(string order)
    {
        var codeBase = Models.CodeBaseOf(order + OrderService);

        var verdict = new DataClass(codeBase).Evaluate(codeBase.Type("Order"));

        Assert.True(verdict.IsSatisfied);
        Assert.Equal("WOC=0 (< 0.33), NOPA+NOAM=6 (> 3), WMC=0 (< 31), NDCM=1 (≥ 1)", verdict.ToString());
    }

    [Fact]
    public void Ignores_Dto_That_Is_Only_Mapped()
    {
        var codeBase = Models.CodeBaseOf(OrderRecord + Mapper);

        var verdict = new DataClass(codeBase).Evaluate(codeBase.Type("OrderLine"));

        Assert.False(verdict.IsSatisfied);
        Assert.Equal("NDCM=0 (≥ 1)", verdict.ToString());
    }

    [Fact]
    public void Ignores_Type_With_Behavior()
    {
        var codeBase = Models.CodeBaseOf(Snippets.Rectangle);

        Assert.False(new DataClass(codeBase).Evaluate(codeBase.Type("Rectangle")).IsSatisfied);
    }

    [Fact]
    public void Data_Clients_Name_The_Methods_Holding_The_Logic()
    {
        var codeBase = Models.CodeBaseOf(OrderRecord + OrderService);

        var client = new DataClients(codeBase).Of(codeBase.Type("Order")).Single();

        Assert.Equal(("OrderService", "CanShip"), (client.Owner.Name, client.Method.Name));
        Assert.Equal(new[] { "P:Order.Status", "P:Order.Total" }, client.AccessedData.OrderBy(id => id));
    }
}
