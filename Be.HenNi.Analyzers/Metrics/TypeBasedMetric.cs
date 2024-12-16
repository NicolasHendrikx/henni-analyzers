using Be.HenNi.Analyzers.Constructions;
using Be.HenNi.Analyzers.Core.Metrics;

namespace Be.HenNi.Analyzers.Metrics;

public abstract class TypeBasedMetric<T> : IMetric<T>
{
    protected TypeDeclaration Operand { get; }
    
    public TypeBasedMetric(TypeDeclaration operand)
    {
        Operand = operand;   
    }

    public abstract T Value { get; }

    public string ForType
        => Operand.SimpleName;
}