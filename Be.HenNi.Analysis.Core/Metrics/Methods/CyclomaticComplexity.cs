using Be.HenNi.Analysis.Core.Model;

namespace Be.HenNi.Analysis.Core.Metrics.Methods;

/// <summary>
/// CYCLO (McCabe): 1 + number of decision points (if, ?:, loops, catch, case labels, non-discard switch arms, &amp;&amp;, ||, ??, ?.).
/// Lambdas and local functions count for their enclosing method.
/// </summary>
public sealed class CyclomaticComplexity : IMetric<MethodModel>
{
    public string Acronym => "CYCLO";

    public string Name => "Cyclomatic Complexity";

    public string Description => "Number of linearly independent paths through the method.";

    public double? Compute(MethodModel method)
        => method.CyclomaticComplexity;
}
