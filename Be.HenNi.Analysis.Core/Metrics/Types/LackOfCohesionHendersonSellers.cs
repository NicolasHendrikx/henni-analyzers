using System.Linq;
using Be.HenNi.Analysis.Core.Model;

namespace Be.HenNi.Analysis.Core.Metrics.Types;

/// <summary>
/// LCOM-HS (Henderson-Sellers): (mean(μ(A)) - m) / (1 - m), where m is the number of methods and μ(A)
/// the number of methods directly accessing attribute A. 0 is perfectly cohesive, 1 and above is not cohesive.
/// Constructors, static methods and members without a body are ignored.
/// Undefined without attributes or with less than two methods.
/// </summary>
public sealed class LackOfCohesionHendersonSellers : IMetric<TypeModel>
{
    public string Acronym => "LCOM-HS";

    public string Name => "Lack Of Cohesion Of Methods (Henderson-Sellers)";

    public string Description => "Lack of cohesion between methods and attributes, from 0 (cohesive) to 2.";

    public double? Compute(TypeModel type)
    {
        var methods = type.Methods
            .Where(method => !method.IsStatic && !method.IsConstructorLike && method.HasBody)
            .ToList();

        if (type.Attributes.Count == 0 || methods.Count < 2)
        {
            return null;
        }

        var meanAccesses = type.Attributes
            .Average(attribute => methods.Count(method => method.AccessedAttributes.Contains(attribute.Id)));

        return (meanAccesses - methods.Count) / (1 - methods.Count);
    }
}
