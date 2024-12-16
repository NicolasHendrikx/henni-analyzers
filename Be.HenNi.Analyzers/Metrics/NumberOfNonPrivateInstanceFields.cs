using System.Linq;
using Be.HenNi.Analyzers.Constructions;

namespace Be.HenNi.Analyzers.Metrics;

public class NumberOfNonPrivateInstanceFields : TypeBasedMetric<int>
{
    public NumberOfNonPrivateInstanceFields(TypeDeclaration operand) : base(operand)
    { }

    public override int Value
        => Operand
            .ExplicitInstanceFields
            .Count(field => !field.IsPrivate);
}