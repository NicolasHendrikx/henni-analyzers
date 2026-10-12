using System.Linq;

namespace Be.HenNi.Analysis.Core.Metrics;

/// <summary>
/// Sum of several metrics, such as NOPA+NOAM. Undefined as soon as one of them is.
/// </summary>
public sealed class SumOfMetrics<TSubject> : IMetric<TSubject>
{
    private readonly IMetric<TSubject>[] _metrics;

    public SumOfMetrics(params IMetric<TSubject>[] metrics)
    {
        _metrics = metrics;
    }

    public string Acronym
        => string.Join("+", _metrics.Select(metric => metric.Acronym));

    public string Name
        => string.Join(" + ", _metrics.Select(metric => metric.Name));

    public string Description
        => $"Sum of {Acronym}.";

    public double? Compute(TSubject subject)
    {
        double sum = 0;
        foreach (var metric in _metrics)
        {
            if (metric.Compute(subject) is not { } value)
            {
                return null;
            }

            sum += value;
        }

        return sum;
    }
}
