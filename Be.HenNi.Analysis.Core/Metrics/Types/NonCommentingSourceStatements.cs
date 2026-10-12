using System.Linq;
using Be.HenNi.Analysis.Core.Model;

namespace Be.HenNi.Analysis.Core.Metrics.Types;

/// <summary>
/// NCSS: number of statements written in the methods of the type.
/// </summary>
public sealed class NonCommentingSourceStatements : IMetric<TypeModel>
{
    public string Acronym => "NCSS";

    public string Name => "Non Commenting Source Statements";

    public string Description => "Number of statements in the methods of the type.";

    public double? Compute(TypeModel type)
        => type.Methods.Sum(method => method.StatementCount);
}
