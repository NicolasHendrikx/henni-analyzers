using System.Threading.Tasks;
using Xunit;

namespace Be.HenNi.Analyzers.Tests.Analyzers;
using Verifier =
    Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        Be.HenNi.Analyzers.Analyzers.TooManyStatementsAnalyzer>;

public class TooManyStatementsAnalyzerTest
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
     public async Task Should_Raise_Diagnostic_On_Too_Many_Statements()
     {
         var expected = Verifier.Diagnostic()
             .WithSpan(12, 5, 86, 6)
             .WithArguments("UpdateQuality", 15, 28);
         await Verifier.VerifyAnalyzerAsync(GildedRose, expected).ConfigureAwait(false);
     }
}