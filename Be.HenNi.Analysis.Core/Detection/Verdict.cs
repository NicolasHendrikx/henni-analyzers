using System.Collections.Generic;
using System.Linq;

namespace Be.HenNi.Analysis.Core.Detection;

/// <summary>
/// Outcome of a criterion with the measurements that decided it:
/// the satisfied ones when it holds, the failing ones otherwise.
/// </summary>
public sealed class Verdict
{
    public Verdict(bool isSatisfied, IEnumerable<Measurement> evidence)
    {
        IsSatisfied = isSatisfied;
        Evidence = evidence.ToList();
    }

    public bool IsSatisfied { get; }

    public IReadOnlyList<Measurement> Evidence { get; }

    public override string ToString()
        => string.Join(", ", Evidence);
}
