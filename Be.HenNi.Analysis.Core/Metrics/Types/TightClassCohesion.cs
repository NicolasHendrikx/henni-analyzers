using System.Linq;
using Be.HenNi.Analysis.Core.Model;

namespace Be.HenNi.Analysis.Core.Metrics.Types;

/// <summary>
/// TCC (Bieman &amp; Kang): relative number of pairs of visible methods that use at least one common attribute.
/// A method uses an attribute when it accesses it directly or through the methods of its type it calls.
/// Constructors, static methods and members without a body are ignored.
/// Undefined for less than two methods.
/// </summary>
public sealed class TightClassCohesion : IMetric<TypeModel>
{
    public string Acronym => "TCC";

    public string Name => "Tight Class Cohesion";

    public string Description => "Relative number of pairs of visible methods sharing at least one attribute.";

    public double? Compute(TypeModel type)
    {
        var usage = new AttributeUsage(type);
        var used = type.Methods
            .Where(method => method.IsVisible && !method.IsStatic && !method.IsConstructorLike && method.HasBody)
            .Select(usage.Of)
            .ToList();

        if (used.Count < 2)
        {
            return null;
        }

        int pairs = 0, connected = 0;
        for (var i = 0; i < used.Count; i++)
        {
            for (var j = i + 1; j < used.Count; j++)
            {
                pairs++;
                if (used[i].Intersect(used[j]).Any())
                {
                    connected++;
                }
            }
        }

        return (double)connected / pairs;
    }
}
