using System.Threading.Tasks;
using Xunit;

using Verifier =
    Be.HenNi.Analyzers.Tests.Verifiers.CSharpAnalyzerVerifier<
        Be.HenNi.Analyzers.Analyzers.TypeScopedDesignFlawsAnalyzer>;

namespace Be.HenNi.Analyzers.Tests.Analyzers;

public class TypeScopedDesignFlawsAnalyzerTests
{
     private const string GildedRose =@"
using System.Collections.Generic;

public class GildedRose
{
    IList<Item> Items;
    public GildedRose(IList<Item> Items)
    {
        this.Items = Items;
    }

    public void UpdateQuality()
    {
        for (var i = 0; i < Items.Count; i++)
        {
            if (Items[i].Name != ""Aged Brie"" && Items[i].Name != ""Backstage passes to a TAFKAL80ETC concert"")
            {
                if (Items[i].Quality > 0)
                {
                    if (Items[i].Name != ""Sulfuras, Hand of Ragnaros"")
                    {
                        Items[i].Quality = Items[i].Quality - 1;
                    }
                }
            }
            else
            {
                if (Items[i].Quality < 50)
                {
                    Items[i].Quality = Items[i].Quality + 1;

                    if (Items[i].Name == ""Backstage passes to a TAFKAL80ETC concert"")
                    {
                        if (Items[i].SellIn < 11)
                        {
                            if (Items[i].Quality < 50)
                            {
                                Items[i].Quality = Items[i].Quality + 1;
                            }
                        }

                        if (Items[i].SellIn < 6)
                        {
                            if (Items[i].Quality < 50)
                            {
                                Items[i].Quality = Items[i].Quality + 1;
                            }
                        }
                    }
                }
            }

            if (Items[i].Name != ""Sulfuras, Hand of Ragnaros"")
            {
                Items[i].SellIn = Items[i].SellIn - 1;
            }

            if (Items[i].SellIn < 0)
            {
                if (Items[i].Name != ""Aged Brie"")
                {
                    if (Items[i].Name != ""Backstage passes to a TAFKAL80ETC concert"")
                    {
                        if (Items[i].Quality > 0)
                        {
                            if (Items[i].Name != ""Sulfuras, Hand of Ragnaros"")
                            {
                                Items[i].Quality = Items[i].Quality - 1;
                            }
                        }
                    }
                    else
                    {
                        Items[i].Quality = Items[i].Quality - Items[i].Quality;
                    }
                }
                else
                {
                    if (Items[i].Quality < 50)
                    {
                        Items[i].Quality = Items[i].Quality + 1;
                    }
                }
            }
        }
    }
}
public class Item
{
    public string Name { get; set; }
    public int SellIn { get; set; }
    public int Quality { get; set; }

    public override string ToString()
    {
        return this.Name + "", "" + this.SellIn + "", "" + this.Quality;
    }
}
";

    [Fact]
    public async Task Should_Report_Type_With_Too_Many_Fields()
    {
        const string text = @"
public class BadClass
{
    private static readonly int STRUCT_FIELD = 24;
    private int _field = 42;

    public int Age { get; set; } = 0;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
";

        var expected = Verifier.Diagnostic("HE0001")
            .WithLocation(2, 14)
            .WithArguments("BadClass", 2, 4);
        await Verifier.VerifyAnalyzerAsync(text, "dotnet_diagnostic.HE0001.threshold = 2", expected);
    }

    [Fact]
    public async Task Should_Apply_Threshold_Specific_To_The_Kind_Of_Type()
    {
        const string text = @"
public class Person(string firstName, string lastName)
{
    public string FullName => firstName + "" "" + lastName;
}

public record Point(int X, int Y, int Z);
";

        const string editorConfig = """
            dotnet_diagnostic.HE0001.threshold = 1
            dotnet_diagnostic.HE0001.threshold.record = 3
            """;

        var expected = Verifier.Diagnostic("HE0001")
            .WithLocation(2, 14)
            .WithArguments("Person", 1, 2);
        await Verifier.VerifyAnalyzerAsync(text, editorConfig, expected);
    }

    [Fact]
    public async Task Should_Ignore_Type_With_Few_Fields()
    {
        const string text = @"
public struct OkStruct
{
    public const string Message=""Hello World"";

    public int IntPart { get; set; }
    public short DecimalPart { get; set; }
}
";

        await Verifier.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task Should_Ignore_Interface()
    {
        const string text = @"
public interface IAdd
{
  int Sum(int a, int b);
  int Minus(int a, int b);
  int Times(int a, int b);
}
";

        await Verifier.VerifyAnalyzerAsync(text, "dotnet_diagnostic.HE0002.threshold = 1");
    }

    [Fact]
    public async Task Should_Report_Type_With_Too_Many_Methods()
    {
        const string text = @"
public class BadClass
{
    private static readonly int STRUCT_FIELD = 24;
    private System.DateTime _birthday = System.DateTime.Now;

    public BadClass() { }

    public int Age
        => _birthday.Year;

    public string FirstName
        => ""Firstname"";

    public string LastName
        => ""Lastname"";

    public override string ToString() => FirstName + LastName;
}
";

        var expected = Verifier.Diagnostic("HE0002")
            .WithLocation(2, 14)
            .WithArguments("BadClass", 2, 4, 1, 3);
        await Verifier.VerifyAnalyzerAsync(text, "dotnet_diagnostic.HE0002.threshold = 2", expected);
    }

    [Fact]
    public async Task Should_Ignore_Type_With_Few_Methods()
    {
        const string text = @"
public class OkRecord
{
    public System.DateTime Birthday { get; set; }
    public int Age
        => Birthday.Year;
}
";

        await Verifier.VerifyAnalyzerAsync(text, "dotnet_diagnostic.HE0002.threshold = 1");
    }

    [Fact]
    public async Task Should_Warn_When_Type_Has_Non_Private_Instance_Fields()
    {
        const string text = @"
public class Clazz
{
    private readonly string _instanceField = string.Empty;
    protected readonly int wrongField = 1;
    public int anotherOne = 2;
    internal double yetAnother = 4.2;
}
";

        var expected = Verifier.Diagnostic("HE0004")
            .WithLocation(2, 14)
            .WithArguments("Clazz", 3);
        await Verifier.VerifyAnalyzerAsync(text, expected);
    }

    [Fact]
    public async Task Should_Stay_Silent_When_Type_Has_All_Private_Instance_Fields()
    {
        const string text = @"
public record Records
{
    private readonly string _instanceField = string.Empty;
    private readonly int _anotherOne = 0;
}
";

        await Verifier.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task Should_Ignore_Members_Generated_For_A_View()
    {
        const string view = """
            public partial class MainWindow
            {
                private int _clicks;

                public void OnClick() { _clicks++; }
            }
            """;

        // Shape of the code generated by Avalonia for the controls named in MainWindow.axaml.
        const string generated = """
            // <auto-generated />
            public partial class MainWindow
            {
                internal object NameBox;
                internal object FirstNameBox;
                internal object EmailBox;
                internal object PhoneBox;
                internal object SaveButton;
                internal object CancelButton;

                public void InitializeComponent(bool loadXaml = true)
                {
                    NameBox = new object();
                    FirstNameBox = new object();
                    EmailBox = new object();
                    PhoneBox = new object();
                    SaveButton = new object();
                    CancelButton = new object();
                }
            }
            """;

        await Verifier.VerifyAnalyzerAsync(new[] { ("/0/MainWindow.axaml.cs", view), ("/0/MainWindow.g.cs", generated) });
    }

    [Fact]
    public async Task Should_Ignore_Members_Marked_As_Generated()
    {
        const string text = """
            using System.CodeDom.Compiler;

            public partial class PersonViewModel
            {
                [GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators", "8.4.0")]
                public object Name;
            }
            """;

        await Verifier.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task Should_Raise_Diagnostic_On_Too_Many_Statements()
    {
        var expected = Verifier.Diagnostic("HE0005")
            .WithLocation(12, 17)
            .WithArguments("UpdateQuality", 15, 25);
        await Verifier.VerifyAnalyzerAsync(GildedRose, expected);
    }

    [Fact]
    public async Task Should_Apply_Configured_Statement_Threshold()
        => await Verifier.VerifyAnalyzerAsync(GildedRose, "dotnet_diagnostic.HE0005.threshold = 25");
}