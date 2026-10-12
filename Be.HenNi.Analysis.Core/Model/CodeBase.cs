using System.Collections.Generic;
using System.Linq;

namespace Be.HenNi.Analysis.Core.Model;

/// <summary>
/// The types of an analyzed code base, for metrics that look beyond a single type.
/// </summary>
public sealed class CodeBase
{
    private readonly Dictionary<string, TypeModel> _byId;

    public CodeBase(IEnumerable<TypeModel> types)
    {
        Types = types.ToList();
        _byId = Types
            .GroupBy(type => type.Id)
            .ToDictionary(group => group.Key, group => group.First());
    }

    public IReadOnlyList<TypeModel> Types { get; }

    public TypeModel? Find(string typeId)
        => _byId.TryGetValue(typeId, out var type) ? type : null;
}
