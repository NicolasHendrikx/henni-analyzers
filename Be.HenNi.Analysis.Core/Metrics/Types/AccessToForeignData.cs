using System.Linq;
using Be.HenNi.Analysis.Core.Model;

namespace Be.HenNi.Analysis.Core.Metrics.Types;

/// <summary>
/// ATFD (Lanza &amp; Marinescu): number of distinct attributes of unrelated types accessed by the type,
/// directly or through accessors. Ancestors and library types are not considered unrelated.
/// </summary>
public sealed class AccessToForeignData : IMetric<TypeModel>
{
    public string Acronym => "ATFD";

    public string Name => "Access To Foreign Data";

    public string Description => "Number of attributes of other types accessed directly or through accessors.";

    public double? Compute(TypeModel type)
        => type.Methods.SelectMany(method => method.ForeignDataAccesses).Distinct().Count();
}
