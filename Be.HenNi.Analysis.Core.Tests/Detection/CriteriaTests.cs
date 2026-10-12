using System.Linq;
using Be.HenNi.Analysis.Core.Detection;
using Be.HenNi.Analysis.Core.Metrics;
using Xunit;

namespace Be.HenNi.Analysis.Core.Tests.Detection;

public class CriteriaTests
{
    private sealed class Constant(string acronym, double? value) : IMetric<object>
    {
        public string Acronym => acronym;
        public string Name => acronym;
        public string Description => acronym;
        public double? Compute(object subject) => value;
    }

    private static Criterion<object> Above(string acronym, double? value, double threshold)
        => Criteria.Metric<object>(new Constant(acronym, value)).GreaterThan(threshold);

    [Fact]
    public void Conjunction_Holds_With_All_Measurements_As_Evidence()
    {
        var verdict = (Above("A", 3, 2) & Above("B", 5, 4)).Evaluate(new object());

        Assert.True(verdict.IsSatisfied);
        Assert.Equal(new[] { "A", "B" }, verdict.Evidence.Select(m => m.Acronym));
    }

    [Fact]
    public void Conjunction_Fails_With_The_First_Failing_Measurement_As_Evidence()
    {
        var verdict = (Above("A", 1, 2) & Above("B", 5, 4)).Evaluate(new object());

        Assert.False(verdict.IsSatisfied);
        Assert.Equal(new[] { "A" }, verdict.Evidence.Select(m => m.Acronym));
    }

    [Fact]
    public void Disjunction_Holds_With_The_Satisfied_Branch_As_Evidence()
    {
        var verdict = (Above("A", 1, 2) | Above("B", 5, 4)).Evaluate(new object());

        Assert.True(verdict.IsSatisfied);
        Assert.Equal(new[] { "B" }, verdict.Evidence.Select(m => m.Acronym));
    }

    [Fact]
    public void Undefined_Metric_Never_Satisfies_A_Threshold()
    {
        Assert.False(Above("A", null, 0).Evaluate(new object()).IsSatisfied);
        Assert.False(Criteria.Metric<object>(new Constant("A", null)).LessThan(1).Evaluate(new object()).IsSatisfied);
    }

    [Fact]
    public void Measurement_Reads_Like_A_Diagnostic()
        => Assert.Equal("TCC=0.12 (< 0.33)", new Measurement("TCC", 0.1234, Comparison.LessThan, 1.0 / 3).ToString());
}
