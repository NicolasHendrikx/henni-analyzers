using Be.HenNi.Analysis.Core.Metrics.Types;
using Be.HenNi.Analysis.Core.Model;

namespace Be.HenNi.Analysis.Core.Detection.Smells;

/// <summary>
/// God Class (Lanza &amp; Marinescu): ATFD &gt; FEW ∧ WMC ≥ VERY HIGH ∧ TCC &lt; ONE THIRD.
/// The class uses a lot of foreign data, is very complex and is not cohesive.
/// Default thresholds are those of the PMD implementation of the strategy.
/// </summary>
public sealed class GodClass : IDetectionStrategy<TypeModel>
{
    public double FewForeignData { get; set; } = 5;

    public double VeryHighComplexity { get; set; } = 47;

    public double LowCohesion { get; set; } = 1.0 / 3;

    public string Name => "God Class";

    public string Description
        => "The type centralizes the intelligence of the system: it is complex, not cohesive and uses data of other types.";

    public Verdict Evaluate(TypeModel type)
        => (Criteria.Metric(new AccessToForeignData()).GreaterThan(FewForeignData)
            & Criteria.Metric(new WeightedMethodCount()).GreaterOrEqual(VeryHighComplexity)
            & Criteria.Metric(new TightClassCohesion()).LessThan(LowCohesion))
            .Evaluate(type);
}
