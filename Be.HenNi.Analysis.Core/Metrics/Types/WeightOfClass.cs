using System.Linq;
using Be.HenNi.Analysis.Core.Model;

namespace Be.HenNi.Analysis.Core.Metrics.Types;

/// <summary>
/// WOC (Lanza &amp; Marinescu): number of functional (non-accessor) public methods divided by the number of public members.
/// Public members are the non-private methods, constructors excluded, and the non-private fields.
/// Undefined when the type exposes no member.
/// </summary>
public sealed class WeightOfClass : IMetric<TypeModel>
{
    public string Acronym => "WOC";

    public string Name => "Weight Of Class";

    public string Description => "Share of the public interface providing services rather than data.";

    public double? Compute(TypeModel type)
    {
        var publicMethods = type.Methods.Where(method => method.IsVisible && !method.IsConstructorLike).ToList();
        var publicMembers = publicMethods.Count + (int)new NumberOfPublicAttributes().Compute(type)!;
        if (publicMembers == 0)
        {
            return null;
        }

        return (double)publicMethods.Count(method => !method.IsAccessor) / publicMembers;
    }
}
