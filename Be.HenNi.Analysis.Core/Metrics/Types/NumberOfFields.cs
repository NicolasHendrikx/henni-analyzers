using Be.HenNi.Analysis.Core.Model;

namespace Be.HenNi.Analysis.Core.Metrics.Types;

/// <summary>
/// NOF: number of instance attributes (fields, auto-properties, captured primary constructor parameters).
/// Static fields and constants are excluded.
/// </summary>
public sealed class NumberOfFields : IMetric<TypeModel>
{
    public string Acronym => "NOF";

    public string Name => "Number Of Fields";

    public string Description => "Number of instance attributes of the type.";

    public double? Compute(TypeModel type)
        => type.Attributes.Count;
}
