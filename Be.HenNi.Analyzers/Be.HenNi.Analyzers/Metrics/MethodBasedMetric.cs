using Be.HenNi.Analyzers.Constructions;
using Be.HenNi.Analyzers.Core.Metrics;

namespace Be.HenNi.Analyzers.Metrics;

public abstract class MethodBasedMetric<T> : IMetric<T>
{
    private readonly MethodDeclaration _method;

    protected MethodBasedMetric(MethodDeclaration method)
    {
        _method = method;
    }

    public string ForType
        => _method.Identifier;
    
    protected MethodDeclaration Operand
        => _method;
    
    public abstract T Value { get; }
}