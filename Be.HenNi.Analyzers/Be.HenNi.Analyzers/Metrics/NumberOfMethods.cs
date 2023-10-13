using System.Linq;

using Be.HenNi.Analyzers.Constructions;

namespace Be.HenNi.Analyzers.Metrics;

public class NumberOfMethods : TypeBasedMetric<int>
{
    public NumberOfMethods(TypeConstruction type) : base(type)
    {
    }

    public override int Value
        => Operand.Methods.Count()
           + Operand.Properties.Count()
           + Operand.Indexers.Count()
           + Operand.Events.Count();
}