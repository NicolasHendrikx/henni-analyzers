using System.Linq;
using Be.HenNi.Analysis.Core.Model;

namespace Be.HenNi.Analysis.Core.Metrics.Types;

/// <summary>
/// NOAM (Lanza &amp; Marinescu): number of non-private accessor methods (getters and setters).
/// Each accessor of a property counts once: <c>{ get; set; }</c> counts 2, <c>{ get; private set; }</c> counts 1.
/// </summary>
public sealed class NumberOfAccessorMethods : IMetric<TypeModel>
{
    public string Acronym => "NOAM";

    public string Name => "Number Of Accessor Methods";

    public string Description => "Number of non-private getters and setters.";

    public double? Compute(TypeModel type)
        => type.Methods.Count(method => method.IsAccessor && method.IsVisible);
}
