using System.Linq;
using Be.HenNi.Analysis.Core.Model;
using Xunit;

namespace Be.HenNi.Analysis.Core.Tests.Model;

public class TypeModelBuilderTests
{
    [Fact]
    public void Attributes_Are_Instance_Fields_And_Auto_Properties()
    {
        var person = Models.Of(Snippets.Person, "Person");

        Assert.Equal(
            new[] { ("_id", AttributeKind.Field, Visibility.Private), ("Age", AttributeKind.Field, Visibility.Public), ("Name", AttributeKind.AutoProperty, Visibility.Private) },
            person.Attributes.Select(a => (a.Name, a.Kind, a.Visibility)).OrderBy(a => a.Name));
    }

    [Fact]
    public void Record_Positional_Parameters_Are_Auto_Properties_With_Accessors()
    {
        var point = Models.Of(Snippets.Point, "Point");

        Assert.Equal(TypeModelKind.Record, point.Kind);
        Assert.Equal(new[] { "X", "Y" }, point.Attributes.Select(a => a.Name));
        Assert.All(point.Attributes, a => Assert.Equal(AttributeKind.AutoProperty, a.Kind));
        Assert.Equal(4, point.Methods.Count(m => m.IsAutoAccessor));
    }

    [Fact]
    public void Synthesized_Record_Members_Are_Not_Modelled()
    {
        var point = Models.Of(Snippets.Point, "Point");

        Assert.DoesNotContain(point.Methods, m => m.Name is "Equals" or "GetHashCode" or "ToString" or "Deconstruct" or "PrintMembers");
    }

    [Fact]
    public void Only_Captured_Primary_Constructor_Parameters_Are_Attributes()
    {
        var greeter = Models.Of(Snippets.Greeter, "Greeter");

        Assert.Equal(
            new[] { ("_n", AttributeKind.Field), ("name", AttributeKind.PrimaryConstructorParameter) },
            greeter.Attributes.Select(a => (a.Name, a.Kind)).OrderBy(a => a.Name));
        Assert.Equal(2, greeter.Method("Greet").AccessedAttributes.Count);
    }

    [Theory]
    [InlineData("get_Balance", true)]
    [InlineData("set_Balance", true)]
    [InlineData("GetOwner", true)]
    [InlineData("SetOwner", true)]
    [InlineData("get_Label", true)]
    [InlineData("set_Label", true)]
    [InlineData("Deposit", false)]
    [InlineData("get_Doubled", false)]
    public void Recognizes_Accessors(string method, bool isAccessor)
    {
        var account = Models.Of(Snippets.Account, "Account");

        Assert.Equal(isAccessor, account.Method(method).IsAccessor);
    }

    [Fact]
    public void Accessor_Visibility_Is_Its_Own()
    {
        var account = Models.Of(Snippets.Account, "Account");

        Assert.Equal(Visibility.Public, account.Method("get_Label").Visibility);
        Assert.Equal(Visibility.Private, account.Method("set_Label").Visibility);
    }

    [Fact]
    public void Implicit_Constructor_Is_Not_Modelled()
    {
        var account = Models.Of(Snippets.Account, "Account");

        Assert.DoesNotContain(account.Methods, m => m.IsConstructorLike);
    }

    [Fact]
    public void Foreign_Data_Are_Attributes_Of_Unrelated_Source_Types()
    {
        var order = Models.Of(Snippets.Order, "Shop.Order");

        Assert.Equal(
            new[] { "F:Shop.Customer.Name", "F:Shop.Customer._email", "P:Shop.Customer.Points" },
            order.Method("Describe").ForeignDataAccesses.Select(a => a.MemberId).OrderBy(id => id, System.StringComparer.Ordinal));
        Assert.All(order.Method("Describe").ForeignDataAccesses, a => Assert.Equal("T:Shop.Customer", a.ProviderTypeId));
    }

    [Fact]
    public void Own_Attributes_Accessed_Are_Recorded()
    {
        var order = Models.Of(Snippets.Order, "Shop.Order");

        Assert.Equal(new[] { "F:Shop.Order._customer" }, order.Method("Describe").AccessedAttributes);
    }

    [Fact]
    public void Own_Property_Reads_Are_Calls_To_Its_Getter()
    {
        var rectangle = Models.Of(Snippets.Rectangle, "Rectangle");

        Assert.Contains("M:Rectangle.get_Formatted", rectangle.Method("Describe").CalledMethods);
        Assert.Contains("M:Rectangle.Area", rectangle.Method("get_Formatted").CalledMethods);
    }

    [Theory]
    [InlineData("Compute", 13)]
    [InlineData("Name", 3)]
    public void Computes_Cyclomatic_Complexity(string method, int expected)
    {
        var flow = Models.Of(Snippets.Flow, "Flow");

        Assert.Equal(expected, flow.Method(method).CyclomaticComplexity);
    }

    [Theory]
    [InlineData("Compute", 16)]
    [InlineData("Name", 1)]
    public void Counts_Statements(string method, int expected)
    {
        var flow = Models.Of(Snippets.Flow, "Flow");

        Assert.Equal(expected, flow.Method(method).StatementCount);
    }
}
