using System.Collections.Generic;
using System.Linq;
using Be.HenNi.Analysis.Core.Model;

namespace Be.HenNi.Analysis.Core.Metrics;

/// <summary>
/// Attributes used by a method directly or through the methods of its own type it calls.
/// </summary>
internal sealed class AttributeUsage
{
    private readonly Dictionary<string, MethodModel> _methods;
    private readonly Dictionary<string, HashSet<string>> _usages = new();

    public AttributeUsage(TypeModel type)
    {
        _methods = type.Methods
            .GroupBy(method => method.Id)
            .ToDictionary(group => group.Key, group => group.First());
    }

    public IReadOnlyCollection<string> Of(MethodModel method)
        => Of(method, new HashSet<string>());

    private HashSet<string> Of(MethodModel method, HashSet<string> visiting)
    {
        if (_usages.TryGetValue(method.Id, out var known))
        {
            return known;
        }

        var used = new HashSet<string>(method.AccessedAttributes);
        visiting.Add(method.Id);
        foreach (var called in method.CalledMethods)
        {
            if (!visiting.Contains(called) && _methods.TryGetValue(called, out var callee))
            {
                used.UnionWith(Of(callee, visiting));
            }
        }

        visiting.Remove(method.Id);
        _usages[method.Id] = used;
        return used;
    }
}
