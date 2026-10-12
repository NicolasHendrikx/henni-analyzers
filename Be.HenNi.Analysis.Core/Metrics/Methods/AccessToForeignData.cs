using Be.HenNi.Analysis.Core.Model;

namespace Be.HenNi.Analysis.Core.Metrics.Methods;

/// <summary>
/// ATFD (Lanza &amp; Marinescu) at method level: number of distinct attributes of unrelated types accessed by the method.
/// </summary>
public sealed class AccessToForeignData : IMetric<MethodModel>
{
    public string Acronym => "ATFD";

    public string Name => "Access To Foreign Data";

    public string Description => "Number of attributes of other types accessed directly or through accessors.";

    public double? Compute(MethodModel method)
        => method.ForeignDataAccesses.Count;
}
