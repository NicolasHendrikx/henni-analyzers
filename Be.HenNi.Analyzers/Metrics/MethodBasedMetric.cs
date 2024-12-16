using Be.HenNi.Analyzers.Constructions;
using Be.HenNi.Analyzers.Core.Metrics;

namespace Be.HenNi.Analyzers.Metrics;

public abstract class MethodBasedMetric<T> : IMetric<T>
{
    private readonly Operation _operation;

    protected MethodBasedMetric(Operation operation)
    {
        _operation = operation;
    }

    public string ForType
        => _operation.Identifier;
    
    protected Operation Operand
        => _operation;
    
    public abstract T Value { get; }
}