using System.Linq;
using Be.HenNi.Analysis.Core.Model;

namespace Be.HenNi.Analysis.Core.Metrics.Types;

/// <summary>
/// NOM: number of operations declared by the type, constructors, finalizers and auto-property accessors excluded.
/// </summary>
public sealed class NumberOfMethods : IMetric<TypeModel>
{
    public string Acronym => "NOM";

    public string Name => "Number Of Methods";

    public string Description => "Number of methods, operators and written accessors declared by the type.";

    public double? Compute(TypeModel type)
        => type.Methods.Count(method => !method.IsConstructorLike && !method.IsAutoAccessor);
}
