using System.Globalization;

namespace Be.HenNi.Analysis.Core.Detection;

public enum Comparison
{
    LessThan,
    LessOrEqual,
    GreaterThan,
    GreaterOrEqual
}

/// <summary>
/// A metric value compared to a threshold, kept as evidence of a verdict.
/// </summary>
public sealed class Measurement
{
    public Measurement(string acronym, double? value, Comparison comparison, double threshold)
    {
        Acronym = acronym;
        Value = value;
        Comparison = comparison;
        Threshold = threshold;
    }

    public string Acronym { get; }

    /// <summary>Null when the metric is undefined for the subject; an undefined value never satisfies a threshold.</summary>
    public double? Value { get; }

    public Comparison Comparison { get; }

    public double Threshold { get; }

    public bool IsSatisfied
        => Value is { } value && Comparison switch
        {
            Comparison.LessThan => value < Threshold,
            Comparison.LessOrEqual => value <= Threshold,
            Comparison.GreaterThan => value > Threshold,
            _ => value >= Threshold
        };

    public override string ToString()
        => $"{Acronym}={Format(Value)} ({Symbol} {Format(Threshold)})";

    private string Symbol
        => Comparison switch
        {
            Comparison.LessThan => "<",
            Comparison.LessOrEqual => "≤",
            Comparison.GreaterThan => ">",
            _ => "≥"
        };

    private static string Format(double? value)
        => value?.ToString("0.##", CultureInfo.InvariantCulture) ?? "n/a";
}
