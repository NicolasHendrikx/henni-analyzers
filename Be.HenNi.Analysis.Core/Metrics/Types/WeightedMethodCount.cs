using System.Linq;
using Be.HenNi.Analysis.Core.Model;

namespace Be.HenNi.Analysis.Core.Metrics.Types;

/// <summary>
/// WMC (Chidamber &amp; Kemerer, weighted by McCabe's complexity as in Lanza &amp; Marinescu):
/// sum of the cyclomatic complexity of the methods having a body.
/// Auto-property accessors and abstract members carry no written code and are excluded.
/// </summary>
public sealed class WeightedMethodCount : IMetric<TypeModel>
{
    public string Acronym => "WMC";

    public string Name => "Weighted Method Count";

    public string Description => "Sum of the cyclomatic complexity of the methods of the type.";

    public double? Compute(TypeModel type)
        => type.Methods.Where(method => method.HasBody).Sum(method => method.CyclomaticComplexity);
}
