using Be.HenNi.Analysis.Core.Metrics;
using Be.HenNi.Analysis.Core.Metrics.Types;
using Be.HenNi.Analysis.Core.Model;

namespace Be.HenNi.Analysis.Core.Detection.Smells;

/// <summary>
/// Data Class (Lanza &amp; Marinescu):
/// WOC &lt; ONE THIRD ∧ ((NOPA+NOAM &gt; FEW ∧ WMC &lt; HIGH) ∨ (NOPA+NOAM &gt; MANY ∧ WMC &lt; VERY HIGH)),
/// completed by NDCM ≥ 1: some method of another type contains logic based on its data.
/// A type exposing data that nobody exploits elsewhere (a DTO only mapped or serialized) is not reported.
/// Classes, structs and records are treated alike.
/// Default thresholds are those of the PMD implementation of the strategy.
/// </summary>
public sealed class DataClass : IDetectionStrategy<TypeModel>
{
    private readonly CodeBase _codeBase;

    public DataClass(CodeBase codeBase)
    {
        _codeBase = codeBase;
    }

    public double LowFunctionality { get; set; } = 1.0 / 3;

    public double FewPublicData { get; set; } = 3;

    public double ManyPublicData { get; set; } = 5;

    public double HighComplexity { get; set; } = 31;

    public double VeryHighComplexity { get; set; } = 47;

    public double MinDataClients { get; set; } = 1;

    public int MinAccessedAttributesByClient { get; set; } = 2;

    public int MinComplexityOfClient { get; set; } = 2;

    public string Name => "Data Class";

    public string Description
        => "The type holds data without behavior, while other types contain the logic based on these data.";

    public Verdict Evaluate(TypeModel type)
    {
        var publicData = new SumOfMetrics<TypeModel>(new NumberOfPublicAttributes(), new NumberOfAccessorMethods());
        var complexity = new WeightedMethodCount();
        var clients = new DataClients(_codeBase)
        {
            MinAccessedAttributes = MinAccessedAttributesByClient,
            MinComplexity = MinComplexityOfClient
        };

        return (Criteria.Metric(new WeightOfClass()).LessThan(LowFunctionality)
                & (Criteria.Metric(publicData).GreaterThan(FewPublicData) & Criteria.Metric(complexity).LessThan(HighComplexity)
                   | Criteria.Metric(publicData).GreaterThan(ManyPublicData) & Criteria.Metric(complexity).LessThan(VeryHighComplexity))
                & Criteria.Metric(clients).GreaterOrEqual(MinDataClients))
            .Evaluate(type);
    }
}
