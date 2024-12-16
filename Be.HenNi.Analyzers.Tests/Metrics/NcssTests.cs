using System.Linq;

using Be.HenNi.Analyzers.Constructions;
using Be.HenNi.Analyzers.Metrics;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace Be.HenNi.Analyzers.Tests.Metrics;

public class NcssTests
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

    const string PasswordSuggester = @"
using System;

internal class PasswordSuggester
{
    static string GetSuggestion()
    {
        int l = 2;
        int u = 3;
        int d = 4;
        int s = 3;

        string str = string.Empty;

        Random randm = new Random();
        string num = ""0123456789"";

        string low_case = ""abcdefghijklmnopqrstuvwxyz"";
        string up_case = ""ABCDEFGHIJKLMNOPQRSTUVWXYZ"";
        string spl_char = ""@#$_()!"";

        int pos = 0;

        if (l == 0)
        {

            pos = randm.Next(1, 1000) % str.Length;

            str = str.Insert(pos, low_case[randm.Next(1,1000) % 26].ToString());
        }

        if (u == 0)
        {
            pos = randm.Next(1, 1000) % str.Length;
            str = str.Insert(pos, up_case[randm.Next(1, 1000) % 26].ToString());
        }

        if (d == 0)
        {
            pos = randm.Next(1, 1000) % str.Length;
            str = str.Insert(pos, num[randm.Next(1, 1000)% 10].ToString());
        }

        if (s == 0)
        {
            pos = randm.Next(1, 1000) % str.Length;
            str = str.Insert(pos, spl_char[randm.Next(1, 1000) % 7].ToString());
        }

        return str;
    }
}
";    
    
    [Theory]
    [InlineData(GildedRose, 28)]
    [InlineData(PasswordSuggester, 24)]
    public void Should_Count_Non_Commenting_Statements(string sourceCode, int expectedValue)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceCode);
        var classRoot =
            tree.GetCompilationUnitRoot()
                .Members
                .OfType<ClassDeclarationSyntax>()
                .First();

        var computed = new NonCommentingSourceStatement(MethodDeclarationSimpleFactory.FromNode(classRoot));
        
        Assert.Equal(expectedValue, computed.Value);
    }
}