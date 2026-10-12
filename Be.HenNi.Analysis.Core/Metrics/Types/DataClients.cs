using System.Collections.Generic;
using System.Linq;
using Be.HenNi.Analysis.Core.Model;

namespace Be.HenNi.Analysis.Core.Metrics.Types;

/// <summary>
/// NDCM: number of methods of other types exploiting the data of the type.
/// A method exploits the data when it accesses at least <see cref="MinAccessedAttributes"/> attributes of the type
/// and contains logic (cyclomatic complexity of at least <see cref="MinComplexity"/>).
/// Mere mapping code (a DTO copied into a domain object) is therefore not a data client.
/// </summary>
/// <remarks>
/// Not a metric from the literature: it captures the coupling Lanza &amp; Marinescu describe between
/// a Data Class and the methods of other classes that envy its data.
/// </remarks>
public sealed class DataClients : IMetric<TypeModel>
{
    private readonly CodeBase _codeBase;

    public DataClients(CodeBase codeBase)
    {
        _codeBase = codeBase;
    }

    public int MinAccessedAttributes { get; set; } = 2;

    public int MinComplexity { get; set; } = 2;

    public string Acronym => "NDCM";

    public string Name => "Number Of Data Client Methods";

    public string Description => "Number of methods of other types containing logic based on the data of the type.";

    public double? Compute(TypeModel type)
        => Of(type).Count;

    public IReadOnlyList<DataClient> Of(TypeModel type)
        => _codeBase.Types
            .Where(owner => owner.Id != type.Id)
            .SelectMany(owner => owner.Methods.Select(method => new DataClient(
                owner,
                method,
                method.ForeignDataAccesses
                    .Where(access => access.ProviderTypeId == type.Id)
                    .Select(access => access.MemberId))))
            .Where(client => client.AccessedData.Count >= MinAccessedAttributes
                             && client.Method.CyclomaticComplexity >= MinComplexity)
            .ToList();
}

public sealed class DataClient
{
    public DataClient(TypeModel owner, MethodModel method, IEnumerable<string> accessedData)
    {
        Owner = owner;
        Method = method;
        AccessedData = accessedData.ToList();
    }

    public TypeModel Owner { get; }

    public MethodModel Method { get; }

    /// <summary>Ids of the attributes or accessors of the data provider used by the method.</summary>
    public IReadOnlyCollection<string> AccessedData { get; }
}
