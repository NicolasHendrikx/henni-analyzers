namespace Be.HenNi.Analyzers.Core.Metrics
{
    public interface IMetric<out T>
    {
        T Value { get; }
        string ForType { get; }
    }    
}

