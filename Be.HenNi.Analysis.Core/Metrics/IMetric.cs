namespace Be.HenNi.Analysis.Core.Metrics;

/// <summary>
/// A metric measured on a type or a method of the code model.
/// </summary>
/// <typeparam name="TSubject">The measured element of the model.</typeparam>
public interface IMetric<in TSubject>
{
    string Acronym { get; }

    string Name { get; }

    string Description { get; }

    /// <returns>The measure, or null when the metric is undefined for <paramref name="subject"/>.</returns>
    double? Compute(TSubject subject);
}
