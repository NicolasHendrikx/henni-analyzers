using System.Linq;
using Be.HenNi.Analysis.Core.Model;

namespace Be.HenNi.Analysis.Core.Metrics.Types;

/// <summary>
/// NOPA (Lanza &amp; Marinescu): number of non-private instance fields.
/// Auto-properties are not counted here: they expose their state through accessors (see NOAM).
/// </summary>
public sealed class NumberOfPublicAttributes : IMetric<TypeModel>
{
    public string Acronym => "NOPA";

    public string Name => "Number Of Public Attributes";

    public string Description => "Number of non-private instance fields.";

    public double? Compute(TypeModel type)
        => type.Attributes.Count(attribute => attribute.Kind == AttributeKind.Field && attribute.IsVisible);
}
