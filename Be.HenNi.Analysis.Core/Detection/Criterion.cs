using System.Linq;
using Be.HenNi.Analysis.Core.Metrics;

namespace Be.HenNi.Analysis.Core.Detection;

/// <summary>
/// A metrics-based condition, composable with <c>&amp;</c> and <c>|</c>, after Marinescu's detection strategies.
/// </summary>
public abstract class Criterion<TSubject>
{
    public abstract Verdict Evaluate(TSubject subject);

    public static Criterion<TSubject> operator &(Criterion<TSubject> left, Criterion<TSubject> right)
        => new AllCriterion(left, right);

    public static Criterion<TSubject> operator |(Criterion<TSubject> left, Criterion<TSubject> right)
        => new AnyCriterion(left, right);

    private sealed class AllCriterion(Criterion<TSubject> left, Criterion<TSubject> right) : Criterion<TSubject>
    {
        public override Verdict Evaluate(TSubject subject)
        {
            var first = left.Evaluate(subject);
            if (!first.IsSatisfied)
            {
                return first;
            }

            var second = right.Evaluate(subject);
            return second.IsSatisfied
                ? new Verdict(true, first.Evidence.Concat(second.Evidence))
                : second;
        }
    }

    private sealed class AnyCriterion(Criterion<TSubject> left, Criterion<TSubject> right) : Criterion<TSubject>
    {
        public override Verdict Evaluate(TSubject subject)
        {
            var first = left.Evaluate(subject);
            if (first.IsSatisfied)
            {
                return first;
            }

            var second = right.Evaluate(subject);
            return second.IsSatisfied
                ? second
                : new Verdict(false, first.Evidence.Concat(second.Evidence));
        }
    }
}

/// <summary>
/// Entry point of the criteria DSL: <c>Criteria.Metric(new WeightedMethodCount()).GreaterOrEqual(47)</c>.
/// </summary>
public static class Criteria
{
    public static ThresholdBuilder<TSubject> Metric<TSubject>(IMetric<TSubject> metric)
        => new(metric);
}

public sealed class ThresholdBuilder<TSubject>
{
    private readonly IMetric<TSubject> _metric;

    internal ThresholdBuilder(IMetric<TSubject> metric)
    {
        _metric = metric;
    }

    public Criterion<TSubject> LessThan(double threshold)
        => new ThresholdCriterion<TSubject>(_metric, Comparison.LessThan, threshold);

    public Criterion<TSubject> LessOrEqual(double threshold)
        => new ThresholdCriterion<TSubject>(_metric, Comparison.LessOrEqual, threshold);

    public Criterion<TSubject> GreaterThan(double threshold)
        => new ThresholdCriterion<TSubject>(_metric, Comparison.GreaterThan, threshold);

    public Criterion<TSubject> GreaterOrEqual(double threshold)
        => new ThresholdCriterion<TSubject>(_metric, Comparison.GreaterOrEqual, threshold);
}

internal sealed class ThresholdCriterion<TSubject>(IMetric<TSubject> metric, Comparison comparison, double threshold)
    : Criterion<TSubject>
{
    public override Verdict Evaluate(TSubject subject)
    {
        var measurement = new Measurement(metric.Acronym, metric.Compute(subject), comparison, threshold);
        return new Verdict(measurement.IsSatisfied, new[] { measurement });
    }
}
