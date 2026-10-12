using Be.HenNi.Analysis.Core.Model;

namespace Be.HenNi.Analysis.Core.Metrics.Methods;

/// <summary>
/// NCSS: number of statements of the method, blocks and empty statements excluded. An expression body counts as one.
/// </summary>
public sealed class NonCommentingSourceStatements : IMetric<MethodModel>
{
    public string Acronym => "NCSS";

    public string Name => "Non Commenting Source Statements";

    public string Description => "Number of statements in the method.";

    public double? Compute(MethodModel method)
        => method.StatementCount;
}
